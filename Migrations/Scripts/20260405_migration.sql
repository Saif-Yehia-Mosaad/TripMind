IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [LocationId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [NameAr] nvarchar(200) NOT NULL,
        [NameEn] nvarchar(200) NOT NULL,
        [Category] nvarchar(450) NOT NULL,
        [Governorate] nvarchar(100) NOT NULL,
        [Latitude] float NOT NULL,
        [Longitude] float NOT NULL,
        [DescriptionAr] nvarchar(max) NULL,
        [DescriptionEn] nvarchar(max) NULL,
        [EntryFeeEgp] decimal(10,2) NULL,
        [AvgPricePerNightEgp] decimal(10,2) NULL,
        [AvgMealPriceEgp] decimal(10,2) NULL,
        [OpeningHours] nvarchar(200) NULL,
        [IsHiddenGem] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PopularityScore] real NOT NULL,
        [AvgRating] real NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [DisplayName] nvarchar(100) NOT NULL,
        [ProfilePhotoUrl] nvarchar(2048) NULL,
        [HomeGovernorate] nvarchar(100) NULL,
        [LanguagePreference] nvarchar(2) NOT NULL DEFAULT N'AR',
        [RememberMe] bit NOT NULL DEFAULT CAST(0 AS bit),
        [GoogleId] nvarchar(128) NULL,
        [FacebookId] nvarchar(128) NULL,
        [PasswordResetToken] nvarchar(512) NULL,
        [ResetTokenExpiry] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [HiddenGems] (
        [HiddenGemId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [LocationId] uniqueidentifier NOT NULL,
        [Story] nvarchar(max) NULL,
        [AnnualVisitors] int NOT NULL,
        [QualityScore] real NOT NULL,
        [TaggedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_HiddenGems] PRIMARY KEY ([HiddenGemId]),
        CONSTRAINT [FK_HiddenGems_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [LocationFeatures] (
        [LocationFeatureId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [LocationId] uniqueidentifier NOT NULL,
        [FeatureKey] nvarchar(100) NOT NULL,
        [FeatureValue] real NOT NULL,
        CONSTRAINT [PK_LocationFeatures] PRIMARY KEY ([LocationFeatureId]),
        CONSTRAINT [FK_LocationFeatures_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [AuditLogId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NULL,
        [EventType] nvarchar(100) NOT NULL,
        [IpAddress] nvarchar(45) NULL,
        [UserAgent] nvarchar(512) NULL,
        [Details] nvarchar(max) NULL,
        [Success] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogId]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [RefreshTokenId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(512) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ReplacedByToken] nvarchar(512) NULL,
        [CreatedByIp] nvarchar(45) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([RefreshTokenId]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [Reviews] (
        [ReviewId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [Rating] int NOT NULL,
        [ReviewText] nvarchar(max) NULL,
        [PhotoUrl] nvarchar(2048) NULL,
        [HelpfulCount] int NOT NULL DEFAULT 0,
        [Reported] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ModerationStatus] nvarchar(max) NOT NULL,
        [VisitedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
        CONSTRAINT [CK_Reviews_Rating] CHECK ([Rating] BETWEEN 1 AND 5),
        CONSTRAINT [FK_Reviews_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [Trips] (
        [TripId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [DestinationGovernorate] nvarchar(100) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [DurationDays] int NOT NULL,
        [TotalBudgetEgp] decimal(12,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [IsPublic] bit NOT NULL DEFAULT CAST(0 AS bit),
        [ShareToken] nvarchar(64) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Trips] PRIMARY KEY ([TripId]),
        CONSTRAINT [FK_Trips_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [UserInterests] (
        [UserInterestId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [InterestTag] nvarchar(100) NOT NULL,
        [Weight] int NOT NULL DEFAULT 1,
        CONSTRAINT [PK_UserInterests] PRIMARY KEY ([UserInterestId]),
        CONSTRAINT [FK_UserInterests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [UserPreferences] (
        [UserPreferenceId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [PreferenceKey] nvarchar(100) NOT NULL,
        [PreferenceValue] nvarchar(500) NOT NULL,
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([UserPreferenceId]),
        CONSTRAINT [FK_UserPreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [ReviewVotes] (
        [ReviewVoteId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [ReviewId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [IsHelpful] bit NOT NULL,
        [VotedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_ReviewVotes] PRIMARY KEY ([ReviewVoteId]),
        CONSTRAINT [FK_ReviewVotes_Reviews_ReviewId] FOREIGN KEY ([ReviewId]) REFERENCES [Reviews] ([ReviewId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReviewVotes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [Budgets] (
        [BudgetId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [TripId] uniqueidentifier NOT NULL,
        [TotalBudgetEgp] decimal(12,2) NOT NULL,
        [AccommodationAllocationEgp] decimal(12,2) NOT NULL,
        [FoodAllocationEgp] decimal(12,2) NOT NULL,
        [TransportAllocationEgp] decimal(12,2) NOT NULL,
        [ActivitiesAllocationEgp] decimal(12,2) NOT NULL,
        [ActualSpentEgp] decimal(12,2) NOT NULL,
        [BudgetVariancePct] real NOT NULL,
        [OptimizerVersion] nvarchar(20) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_Budgets] PRIMARY KEY ([BudgetId]),
        CONSTRAINT [FK_Budgets_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [Trips] ([TripId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [SavedItineraries] (
        [SavedItineraryId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [UserId] uniqueidentifier NOT NULL,
        [TripId] uniqueidentifier NOT NULL,
        [CustomName] nvarchar(200) NULL,
        [IsFavorite] bit NOT NULL DEFAULT CAST(0 AS bit),
        [SavedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_SavedItineraries] PRIMARY KEY ([SavedItineraryId]),
        CONSTRAINT [FK_SavedItineraries_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [Trips] ([TripId]),
        CONSTRAINT [FK_SavedItineraries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [TripDays] (
        [TripDayId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [TripId] uniqueidentifier NOT NULL,
        [DayNumber] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_TripDays] PRIMARY KEY ([TripDayId]),
        CONSTRAINT [FK_TripDays_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [Trips] ([TripId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE TABLE [TripLocations] (
        [TripLocationId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [TripId] uniqueidentifier NOT NULL,
        [TripDayId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [DayNumber] int NOT NULL,
        [TimeSlot] nvarchar(5) NOT NULL,
        [VisitDurationMinutes] int NOT NULL,
        [SequenceOrder] int NOT NULL,
        CONSTRAINT [PK_TripLocations] PRIMARY KEY ([TripLocationId]),
        CONSTRAINT [FK_TripLocations_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([LocationId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TripLocations_TripDays_TripDayId] FOREIGN KEY ([TripDayId]) REFERENCES [TripDays] ([TripDayId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TripLocations_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [Trips] ([TripId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EventType_Created] ON [AuditLogs] ([EventType], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Budgets_TripId] ON [Budgets] ([TripId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HiddenGems_LocationId] ON [HiddenGems] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LocationFeatures_LocationId] ON [LocationFeatures] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Locations_Category] ON [Locations] ([Category]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Locations_Governorate] ON [Locations] ([Governorate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UIX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_LocationId] ON [Reviews] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UIX_Reviews_User_Location] ON [Reviews] ([UserId], [LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReviewVotes_UserId] ON [ReviewVotes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UIX_ReviewVotes_Review_User] ON [ReviewVotes] ([ReviewId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SavedItineraries_TripId] ON [SavedItineraries] ([TripId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SavedItineraries_UserId] ON [SavedItineraries] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TripDays_TripId] ON [TripDays] ([TripId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TripLocations_LocationId] ON [TripLocations] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TripLocations_TripDayId] ON [TripLocations] ([TripDayId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TripLocations_TripId] ON [TripLocations] ([TripId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Trips_UserId] ON [Trips] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UIX_Trips_ShareToken] ON [Trips] ([ShareToken]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserInterests_UserId] ON [UserInterests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UIX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310025613_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260310025613_InitialCreate', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310215548_AddIsActiveToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260310215548_AddIsActiveToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260310215548_AddIsActiveToUser', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'IsActive');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [EmailOtpExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [EmailVerificationOtp] nvarchar(6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [FailedLoginAttempts] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [IsEmailVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutEnd] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorOtp] nvarchar(6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorOtpExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402124028_AddAuthSecurityFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402124028_AddAuthSecurityFields', N'8.0.7');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403115049_FixOtpColumnLength'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'TwoFactorOtp');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Users] ALTER COLUMN [TwoFactorOtp] nvarchar(512) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403115049_FixOtpColumnLength'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'EmailVerificationOtp');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Users] ALTER COLUMN [EmailVerificationOtp] nvarchar(512) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403115049_FixOtpColumnLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403115049_FixOtpColumnLength', N'8.0.7');
END;
GO

COMMIT;
GO

