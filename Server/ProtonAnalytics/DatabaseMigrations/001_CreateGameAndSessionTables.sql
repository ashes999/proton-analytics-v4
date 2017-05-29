create table Games (
	Id uniqueidentifier primary key not null default newid(),
	Name nvarchar(255) not null,
	ApiKey varchar(64) not null, -- 32-bits base-64 encoded
	OwnerId nvarchar(128) foreign key references AspNetUsers(Id),
	CreatedOnUtc datetime not null default getutcdate()	
)

create table Sessions (
	Id uniqueidentifier primary key not null default newid(),
	GameId uniqueidentifier foreign key references Games(Id),
	PlayerId uniqueidentifier not null,
	SessionStartUtc datetime not null,
	SessionEndUtc datetime, -- null if we don't know, eg. on Flash
	[Platform] varchar(32) not null,
	[OperatingSystem] varchar(32) not null
)

create index IX_Sessions_GamePlayerStart on Sessions(GameId, PlayerId, SessionStartUtc)