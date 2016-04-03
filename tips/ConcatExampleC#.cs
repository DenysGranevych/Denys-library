   IQueryable<IFile> query = DbManager.FileRepository.GetAll().Select(x => (IFile)x)
                        .Concat(DbManager.SliceRepository.GetAll().Select(x => (IFile)x)).AsQueryable();