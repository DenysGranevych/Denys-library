IF (EXISTS
(
    SELECT *
    FROM sysobjects
    WHERE id = OBJECT_ID(N'usp_ProductCategories_CreateUpdate')
          AND OBJECTPROPERTY(id, N'IsProcedure') = 1
)
   )
BEGIN
    DROP PROC usp_ProductCategories_CreateUpdate;
END;
GO

CREATE PROC usp_ProductCategories_CreateUpdate
    @_currentUserId INT,
    @MerchantAccountId INT,
    @ProductCategories ProductCategoryCollection READONLY
AS
BEGIN
    BEGIN TRY
        -- COPY ProductCategoryCollection IN Template
        SELECT *
        INTO #ProductCategoriesTemplate
        FROM @ProductCategories;

        DECLARE @CurrentLevelCategories ProductCategoryCollection;
        DECLARE @HandledCategories TABLE
        (
            ExternalId NVARCHAR(68),
            ParentId INT
        );
        DECLARE @hasInvalidParentKeys BIT = 0;

        -- categories without parent(Null or not exist in Template)
        INSERT INTO @CurrentLevelCategories
        SELECT t.Id,
               t.Name,
               t.ParentId
        FROM #ProductCategoriesTemplate AS t
            LEFT JOIN #ProductCategoriesTemplate AS tparent
                ON t.ParentId = tparent.Id AND tparent.Id IS NULL
        --WHERE tparent.Id IS NULL;

        DECLARE @countIteration INT = 0;
        DECLARE @categoriesCount INT = (
                                           SELECT COUNT(*) FROM #ProductCategoriesTemplate
                                       );

        BEGIN TRAN;

        WHILE @countIteration < @categoriesCount
              AND (
                  (
                      SELECT COUNT(*) FROM #ProductCategoriesTemplate
                  )   > 0
                  OR
                  (
                      SELECT COUNT(*) FROM @CurrentLevelCategories
                  )   > 0
                  )
        BEGIN
            DELETE FROM @HandledCategories;

            MERGE dbo.ProductCategories AS t
            USING @CurrentLevelCategories AS clc
            ON (t.ExternalId = clc.Id)
            WHEN NOT MATCHED BY TARGET THEN
                INSERT
                (
                    ExternalId,
                    Name,
                    ParentId,
                    MerchantAccountId
                )
                VALUES
                (   clc.Id,
                    clc.Name,
                    (
                        SELECT TOP 1
                            p.Id
                        FROM dbo.ProductCategories p
                        WHERE p.ExternalId = clc.ParentId
                    ),
                    @MerchantAccountId
                )
            WHEN MATCHED THEN
                UPDATE SET t.Name = clc.Name,
                           t.ParentId =
                           (
                               SELECT TOP 1
                                   p.Id
                               FROM dbo.ProductCategories p
                               WHERE p.ExternalId = clc.ParentId
                           )
            OUTPUT Inserted.ExternalId,
                   Inserted.ParentId
            INTO @HandledCategories;

            --validation on ParentId Binding
            DECLARE @NotValidCategories TABLE (id INT);

            INSERT INTO @NotValidCategories
            SELECT h.ExternalId
            FROM @HandledCategories h
                JOIN @CurrentLevelCategories c
                    ON h.ExternalId = c.Id
            WHERE (c.ParentId IS NOT NULL)
                  AND (h.ParentId IS NULL);

            IF ((SELECT COUNT(*) FROM @NotValidCategories) > 0)
            BEGIN
                SET @hasInvalidParentKeys = 1;
                BREAK;
            END;

            --remove handled categories from Template
            DELETE FROM #ProductCategoriesTemplate
            WHERE Id IN (
                            SELECT ExternalId FROM @HandledCategories
                        );

            --new level
            DELETE FROM @CurrentLevelCategories;

            INSERT INTO @CurrentLevelCategories
            SELECT t.Id,
                   t.Name,
                   t.ParentId
            FROM #ProductCategoriesTemplate AS t
            WHERE ParentId IN (
                                          SELECT ExternalId FROM @HandledCategories
                                      );

            SET @countIteration = @countIteration + 1;
        END;
    END TRY
    BEGIN CATCH
        SET @hasInvalidParentKeys = 1;
    END CATCH;
    BEGIN
        IF OBJECT_ID('tempdb.dbo.#ProductCategoriesTemplate', 'U') IS NOT NULL
            DROP TABLE #ProductCategoriesTemplate;
        IF @hasInvalidParentKeys = 1
        BEGIN
            ROLLBACK TRAN;
            DECLARE @errorMessage VARCHAR(3000);
                SET @errorMessage =
            (
                SELECT c.id FROM @NotValidCategories c ORDER BY id FOR XML PATH('')
            )
            SET @errorMessage = 'Not valid ParentId for this categories: ' + @errorMessage;
            RAISERROR(@errorMessage, 16, 1);
        END;
        ELSE
        BEGIN
            COMMIT TRAN;
        END;
    END;
END;