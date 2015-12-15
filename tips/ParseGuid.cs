Guid newGuid;
newGuid = Guid.TryParse(guid, out newGuid) ? new Guid(guid) : Guid.Empty;