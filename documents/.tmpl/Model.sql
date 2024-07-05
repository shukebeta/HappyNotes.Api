select '%TABLE_NAME%'
into @table;
select '%DB_NAME%'
into @schema;

select concat('    public class ', @table, '{')
union
select concat('        public ', tps.dest, ' ', column_name, ' { get; set; }')
from information_schema.columns c
         join (
    select 'char' as orign, 'string' as dest
    union all
    select 'varchar', 'string'
    union all
    select 'timestamp', 'DateTime'
    union all
    select 'datetime', 'DateTime'
    union all
    select 'date', 'DateTime'
    union all
    select 'text', 'string'
    union all
    select 'binary', 'byte[]'
    union all
    select 'int', 'int'
    union all
    select 'decimal', 'decimal'
    union all
    select 'float', 'float'
    union all
    select 'tinyint', 'int'
    union all
    select 'bigint', 'long'
) tps on c.data_type like tps.orign
where table_schema = @schema
  and table_name = @table
union
select '}';
