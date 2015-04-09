CREATE TABLE [dbo].[ListingAttribute]
(
	[ListingID] BIGINT NOT NULL , 
    [Name] VARCHAR(255) NOT NULL, 
    [Value] VARCHAR(255) NOT NULL, 
    [AttributeID] INT NOT NULL IDENTITY, 
    CONSTRAINT [FK_ListingAttribute_Listing] FOREIGN KEY ([ListingID]) REFERENCES [Listing]([Id]) ON DELETE CASCADE, 
    CONSTRAINT [PK_ListingAttribute] PRIMARY KEY ([AttributeID]) 
)
