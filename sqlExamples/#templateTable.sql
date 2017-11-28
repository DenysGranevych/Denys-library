--CREATE TYPE SoonestExpirationAccount AS TABLE
--(
--    AccountId INT NOT NULL,
--    SubscriptionPlanTypeId INT NOT NULL,
--    ExpirationDate INT NOT NULL
--);

DECLARE @MerchantId INT = 107;

DECLARE @AccountsCount INT =
	(SELECT COUNT(Id) FROM MerchantAccounts AS a WHERE MerchantId = @MerchantId AND a.IsDeleted = 0);

	SELECT TOP 1
	    a.Id,
	    a.ExpirationDate,
	    a.SubscriptionPlanTypeId
	INTO #SoonestExpirationAccount
	FROM MerchantAccounts AS a
	WHERE MerchantId = @MerchantId
	      AND a.IsDeleted = 0
	      AND a.IsCanceled = 0
	ORDER BY a.ExpirationDate DESC;

	SELECT TOP 1
	    m.Id AS Id,
	    m.Name AS Name,
	    m.Logo AS Logo,
	    @AccountsCount AS AccountsCount,
	    (SELECT TOP 1 SubscriptionPlanTypeId FROM #SoonestExpirationAccount) AS SoonestExpirationAccountSubscriptionPlanTypeId,
	    (SELECT TOP 1 Id FROM #SoonestExpirationAccount) AS SoonestExpirationAccountId,
	    (SELECT TOP 1 ExpirationDate FROM #SoonestExpirationAccount) AS SoonestExpirationAccountExpirationDate

	FROM Merchants AS m
	WHERE m.Id = @MerchantId;

	DROP TABLE #SoonestExpirationAccount;