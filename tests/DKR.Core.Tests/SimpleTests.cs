using Xunit;
using DKR.Core.Entities;
using DKR.Shared.Enums;

namespace DKR.Core.Tests;

public class SimpleTests
{
    [Fact]
    public void Client_CanBeCreated()
    {
        // Arrange & Act
        var client = new Client
        {
            UUID = "KL-2024-0001",
            Gender = Gender.Male,
            BirthYear = 1990,
            MainSubstance = SubstanceType.Heroin
        };

        // Assert
        Assert.NotNull(client);
        Assert.Equal("KL-2024-0001", client.UUID);
        Assert.Equal(Gender.Male, client.Gender);
    }

    [Fact]
    public void Session_CanBeCreated()
    {
        // Arrange & Act
        var session = new Session
        {
            ClientId = "test-client-id",
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active,
            Room = "1"
        };

        // Assert
        Assert.NotNull(session);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal("1", session.Room);
    }

    [Fact]
    public void EmergencyEvent_CanBeCreated()
    {
        // Arrange & Act
        var emergency = new EmergencyEvent
        {
            Type = Entities.EmergencyType.Overdose,
            ClientId = "test-client",
            Description = "Test emergency",
            OccurredAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(emergency);
        Assert.Equal(Entities.EmergencyType.Overdose, emergency.Type);
        Assert.Equal("test-client", emergency.ClientId);
    }
}