create table Games (
	Id uniqueidentifier primary key not null default newid(),
	Name nvarchar(255) not null,
	ApiKey varchar(max) not null,
	OwnerId nvarchar(128) foreign key references AspNetUsers(Id),
	CreatedOn datetime not null default getutcdate()	
)