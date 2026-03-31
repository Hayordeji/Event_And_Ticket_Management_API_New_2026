IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Identity')
    CREATE DATABASE TicketingSystem_Identity;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Finance')
    CREATE DATABASE TicketingSystem_Finance;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Catalog')
    CREATE DATABASE TicketingSystem_Catalog;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Sales')
    CREATE DATABASE TicketingSystem_Sales;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Fulfillment')
    CREATE DATABASE TicketingSystem_Fulfillment;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TicketingSystem_Access')
    CREATE DATABASE TicketingSystem_Access;