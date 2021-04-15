using System.Linq;

internal static class CatalystHelper
{
    public static string ToUrlParam(DeploymentOptions deploymentOptions)
    {
        string result = ToUrlParam(deploymentOptions.filters);

        if (!string.IsNullOrEmpty(deploymentOptions.sortBy))
            result += $"&sortingField={deploymentOptions.sortBy}";

        if (!string.IsNullOrEmpty(deploymentOptions.sortOrder))
            result += $"&sortingOrder={deploymentOptions.sortOrder}";

        if (!string.IsNullOrEmpty(deploymentOptions.lastId))
            result += $"&lastId={deploymentOptions.lastId}";

        if (deploymentOptions.offset != null)
            result += $"&offset={deploymentOptions.offset}";

        if (deploymentOptions.limit != null)
            result += $"&limit={deploymentOptions.limit}";

        return result;
    }

    public static string ToUrlParam(DeploymentFilters deploymentFilters)
    {
        if (deploymentFilters == null)
        {
            return string.Empty;
        }

        string result = $"onlyCurrentlyPointed={(deploymentFilters.onlyCurrentlyPointed ? "true" : "false")}";

        if (deploymentFilters.pointers != null)
        {
            result = deploymentFilters.pointers.Aggregate(result, (current, pointer) => current + $"&pointer={pointer}");
        }
        if (deploymentFilters.entityIds != null)
        {
            result = deploymentFilters.entityIds.Aggregate(result, (current, entityId) => current + $"&entityId={entityId}");
        }
        if (deploymentFilters.entityTypes != null)
        {
            result = deploymentFilters.entityTypes.Aggregate(result, (current, entityType) => current + $"&entityType={entityType}");
        }
        if (deploymentFilters.deployedBy != null)
        {
            result = deploymentFilters.deployedBy.Aggregate(result, (current, address) => current + $"&deployedBy={address}");
        }

        return result;
    }
}