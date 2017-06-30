-- delete sessions first, it's just test data up to here
alter table sessions
add [Version] varchar(32) not null