﻿CREATE TABLE [dbo].[ListingAttribute]
(
	[ListingID] BIGINT NOT NULL , 
    [Name] VARCHAR(255) NOT NULL, 
    [Value] VARCHAR(255) NOT NULL, 
    [Inferred] BIT NOT NULL DEFAULT 0, 
    [AttributeID] INT NOT NULL IDENTITY, 
    CONSTRAINT [FK_ListingAttribute_Listing] FOREIGN KEY ([ListingID]) REFERENCES [Listing]([Id]), 
    CONSTRAINT [PK_ListingAttribute] PRIMARY KEY ([AttributeID]) 
)
