namespace DKR.Core.Interfaces;

public enum DeploymentMode
{
    Cloud,
    OnPremise,
    Hybrid
}

public interface IDeploymentService
{
    DeploymentMode DetectMode();
    bool IsCloudDeployment();
    bool IsOnPremiseDeployment();
    string GetDeploymentRegion();
    bool IsFeatureEnabled(string featureName);
}