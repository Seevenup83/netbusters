#!/bin/bash

PGPASSWORD=mypassword psql -h localhost -U myuser -d mydatabase -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"; 
rm -rf Migrations/;
dotnet ef migrations add InitialCommit;
dotnet ef database update