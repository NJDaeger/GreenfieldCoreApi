-- DependsOn: ScriptHistory
create table if not exists `Connections.PatreonConnections` (
   PatreonConnectionId bigint auto_increment not null primary key unique,
   RefreshToken varchar(512) not null,
   AccessToken varchar(512) not null,
   TokenType varchar(64) not null,
   TokenExpiry datetime not null,
   Scope varchar(1024) not null,
   PatreonId bigint not null,
   FullName varchar(256) not null,
   Pledge decimal null,
   UpdatedOn datetime default null on update current_timestamp null,
   CreatedOn datetime default current_timestamp not null,
   constraint UQ_PatreonConnections_PatreonId unique (PatreonId)
) character set utf8mb4 collate utf8mb4_unicode_ci;