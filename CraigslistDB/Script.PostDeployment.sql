/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
delete from VehicleModel
delete from VehicleMake
delete from CLSiteSection
delete from CLSubCity
delete from CLCity

:r .\InsertVehicleModel.sql
:r .\InsertVehicleMake.sql
:r .\InsertCLSiteSection.sql
:r .\InsertCLCity.sql
:r .\InsertCLSubCity.sql