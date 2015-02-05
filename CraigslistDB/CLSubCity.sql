CREATE TABLE [dbo].[CLSubCity]
(
	[ParentCity] VARCHAR(255) NOT NULL , 
    [SubCity] CHAR(3) NOT NULL, 
    CONSTRAINT [FK_CLSubCity_CLCity] FOREIGN KEY ([ParentCity]) REFERENCES [CLCity]([Name]), 
    CONSTRAINT [PK_CLSubCity] PRIMARY KEY ([SubCity])
)
