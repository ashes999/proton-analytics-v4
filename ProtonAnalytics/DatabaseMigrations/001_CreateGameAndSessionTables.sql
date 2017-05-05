create table Games (
	Id uniqueidentifier primary key not null default newid(),
	Name nvarchar(255) not null,
	ApiKey varchar(max) not null,
	OwnerId nvarchar(128) foreign key references AspNetUsers(Id),
	CreatedOn datetime not null default getutcdate()	
)

create table Sessions (
	PlayerId uniqueidentifier not null,
	SessionStart datetime not null,
	SessionEnd datetime, -- null if we don't know, eg. on Flash
	[Platform] varchar(32) not null,
	primary key(PlayerId, SessionStart)
)