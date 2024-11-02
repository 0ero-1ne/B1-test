use task1;
go

drop procedure if exists dbo.calculate;
go

create procedure dbo.calculate
as
begin
	declare @count  bigint = (select count(*) from dbo.useful_data);

	-- сдвинем выборку до центра а дальше возьмём ещё одну строку, если количество строк чётное
	declare @median float = (
		select avg(1.0 * float_number)
		from (
			select useful_data.float_number FROM dbo.useful_data
			order by float_number
			offset (@count - 1) / 2 rows
			fetch next 1 + (1 - @count % 2) rows only
		) as x
	);

	select sum(useful_data.integer_number) as 'Sum', @median as 'Median' from useful_data;
	return 1;
end