CREATE TABLE [dbo].[Listing]
(
	[Id] BIGINT NOT NULL PRIMARY KEY, 
    [Body] VARCHAR(MAX) NOT NULL, 
    [Title] VARCHAR(255) NOT NULL, 
    [PostDate] DATETIME NOT NULL, 
    [SiteSection] CHAR(3) NOT NULL, 
    [Timestamp] DATETIME NOT NULL, 
    [City] VARCHAR(255) NOT NULL, 
    [SubCity] CHAR(3) NULL, 
    CONSTRAINT [FK_Listing_CLCity] FOREIGN KEY ([City]) REFERENCES [CLCity]([Name]), 
    CONSTRAINT [FK_Listing_CLSiteSection] FOREIGN KEY ([SiteSection]) REFERENCES [CLSiteSection]([Name]), 
    CONSTRAINT [FK_Listing_CLSubCity] FOREIGN KEY ([SubCity]) REFERENCES [CLSubCity]([SubCity])
)
