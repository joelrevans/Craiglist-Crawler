﻿CREATE TABLE [dbo].[Proxy]
(
	[IP] VARCHAR(45) NOT NULL PRIMARY KEY, 
    [Port] INT NOT NULL, 
    [Enabled] BIT NOT NULL , 
    [Cooldown] FLOAT NOT NULL
)
