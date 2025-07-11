using DKR.Core.Interfaces;
using DKR.Shared.Enums;
using Microsoft.Extensions.Configuration;

namespace DKR.Infrastructure.Configuration;

public class DeploymentDetectionService : IDeploymentService
{
    private readonly IConfiguration _configuration;
    private readonly Core.Interfaces.DeploymentMode _deploymentMode;

    public DeploymentDetectionService(IConfiguration configuration)
    {
        _configuration = configuration;
        _deploymentMode = DetectDeploymentMode();
    }

    public Core.Interfaces.DeploymentMode DetectMode() => _deploymentMode;

    public bool IsCloudDeployment() => _deploymentMode == Core.Interfaces.DeploymentMode.Cloud;

    public bool IsOnPremiseDeployment() => _deploymentMode == Core.Interfaces.DeploymentMode.OnPremise;

    public string GetDeploymentRegion() => _configuration["Deployment:Region"] ?? "EU-West";

    public bool IsFeatureEnabled(string featureName)
    {
        var modeString = _deploymentMode.ToString();
        var featurePath = $"Features:{modeString}:{featureName}";
        return _configuration.GetValue<bool>(featurePath);
    }

    private Core.Interfaces.DeploymentMode DetectDeploymentMode()
    {
        var configuredMode = _configuration["Deployment:Mode"];
        
        if (!string.IsNullOrEmpty(configuredMode) && configuredMode != "Auto")
        {
            return Enum.Parse<Core.Interfaces.DeploymentMode>(configuredMode, true);
        }

        // Auto-Detection
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
        {
            return Core.Interfaces.DeploymentMode.Cloud;
        }

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST")))
        {
            return Core.Interfaces.DeploymentMode.Cloud;
        }

        return Core.Interfaces.DeploymentMode.OnPremise;
    }
}