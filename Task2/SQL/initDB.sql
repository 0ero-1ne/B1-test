use master;
go

if exists (select name from master.sys.databases where name = N'task2')
begin
	drop database task2;
end;

use master;
create database task2;
go

use task2;
go

create table banks
(
	id   int not null primary key identity(1, 1),
	name nvarchar(255) not null unique
);

create table bill_types
(
	id    int not null primary key identity(1, 1),
	title nvarchar(255) not null unique
);

insert into bill_types (title)
values (N'Денежные средства, драгоценные металлы и межбанковские операции'),
	   (N'Кредитные и иные активные операции с клиентами'),
	   (N'Счета по операциям клиентов'),
	   (N'Ценные бумаги'),
	   (N'Долгосрочные финансовые вложения в уставные фонды юридических лиц, основные средства и прочее имущество'),
	   (N'Прочие активы и прочие пассивы'),
	   (N'Собственный капитал банка'),
	   (N'Доходы банка'),
	   (N'Расходы банка');

create table files
(
	id		    int not null primary key identity(1, 1),
	name	    nvarchar(255) not null,
	description nvarchar(1024) not null,
	start_date  date not null,
	end_date    date not null,
	bank_id	    int not null foreign key references banks(id)
);

create table bills
(
	id				 bigint not null primary key identity(1, 1),
	book_number		 int not null,
	insaldo_active   numeric(38, 2) not null,
	insaldo_passive  numeric(38, 2) not null,
	turnovers_debit  numeric(38, 2) not null,
	turnovers_credit numeric(38, 2) not null,
	outsaldo_active  numeric(38, 2) not null,
	outsaldo_passive numeric(38, 2) not null,
	bill_type_id     int not null foreign key references bill_types(id),
	file_id			 int not null foreign key references files(id)
);