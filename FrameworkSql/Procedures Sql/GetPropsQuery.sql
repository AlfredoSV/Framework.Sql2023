CREATE or ALTER PROCEDURE GetPropsQuery (@query varchar(max))
as
BEGIN

DECLARE @pasaDato VARCHAR(MAX)

if exists(Select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'tt')
	drop table ttb;

set @pasaDato = 'Select top 0 tmp.* into tt From (' + @query + ') tmp'	 

SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = 'TT'
END;