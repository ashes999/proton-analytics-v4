create table FeatureToggles (
	ToggleName varchar(64) primary key not null,
	IsEnabled bit not null default(1)
)

insert into FeatureToggles (togglename, isenabled) values ('AllowUserRegistrations', 1)