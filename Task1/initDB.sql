use master;
go

if exists (select name from master.sys.databases where name = N'TASK1')
begin
	drop database task1;
end;

use master;
create database task1;
go

use task1;
go

create table useful_data
(
	id			   bigint identity(1,1) not null primary key,
	"date"		   datetime not null,
	english_string nchar(10) not null,
	russian_string nchar(10) not null,
	integer_number int not null,
	float_number   float not null
);