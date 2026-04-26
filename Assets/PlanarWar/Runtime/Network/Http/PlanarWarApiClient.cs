using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace PlanarWar.Client.Network
{
    public sealed class PlanarWarApiClient
    {
        private readonly Func<string> getHttpBaseUrl;
        private readonly Func<string> getBearerToken;

        public PlanarWarApiClient(Func<string> getHttpBaseUrl, Func<string> getBearerToken = null)
        {
            this.getHttpBaseUrl = getHttpBaseUrl ?? throw new ArgumentNullException(nameof(getHttpBaseUrl));
            this.getBearerToken = getBearerToken;
        }

        public Task<JObject> FetchSummaryAsync()
        {
            return GetJsonAsync(BuildUrl("/api/me"));
        }


        public Task<JObject> FetchWorkshopRecipesAsync()
        {
            return GetJsonAsync(BuildUrl("/api/workshop/recipes"));
        }

        public Task<JObject> LoginAsync(string emailOrName, string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrName))
            {
                throw new ArgumentException("emailOrName is required", nameof(emailOrName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("password is required", nameof(password));
            }

            var body = new JObject
            {
                ["emailOrName"] = emailOrName.Trim(),
                ["password"] = password
            };

            return PostJsonAsync(BuildUrl("/api/auth/login"), body, includeBearerToken: false);
        }

        public Task<JObject> StartResearchAsync(string techId)
        {
            if (string.IsNullOrWhiteSpace(techId))
            {
                throw new ArgumentException("techId is required", nameof(techId));
            }

            var body = new JObject
            {
                ["techId"] = techId,
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/tech/start"), body, includeBearerToken: true);
        }

        public Task<JObject> ConstructBuildingAsync(string kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
            {
                throw new ArgumentException("kind is required", nameof(kind));
            }

            var body = new JObject
            {
                ["kind"] = kind.Trim(),
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/buildings/construct"), body, includeBearerToken: true);
        }

        public Task<JObject> UpgradeBuildingAsync(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                throw new ArgumentException("buildingId is required", nameof(buildingId));
            }

            var body = new JObject
            {
                ["buildingId"] = buildingId.Trim(),
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/buildings/upgrade"), body, includeBearerToken: true);
        }

        public Task<JObject> SetBuildingRoutingPreferenceAsync(string buildingId, string routingPreference)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                throw new ArgumentException("buildingId is required", nameof(buildingId));
            }

            if (string.IsNullOrWhiteSpace(routingPreference))
            {
                throw new ArgumentException("routingPreference is required", nameof(routingPreference));
            }

            var body = new JObject
            {
                ["buildingId"] = buildingId.Trim(),
                ["routingPreference"] = routingPreference.Trim()
            };

            return PostJsonAsync(BuildUrl("/api/buildings/routing"), body, includeBearerToken: true);
        }



        public Task<JObject> StartWorkshopCraftAsync(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                throw new ArgumentException("recipeId is required", nameof(recipeId));
            }

            var body = new JObject
            {
                ["recipeId"] = recipeId.Trim(),
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/workshop/craft"), body, includeBearerToken: true);
        }

        public Task<JObject> CollectWorkshopAsync(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentException("jobId is required", nameof(jobId));
            }

            var body = new JObject
            {
                ["jobId"] = jobId.Trim(),
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/workshop/collect"), body, includeBearerToken: true);
        }

        public Task<JObject> RecruitHeroAsync(string role = null)
        {
            var body = new JObject
            {
                ["serviceMode"] = "private_city"
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                body["role"] = role.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/heroes/recruit"), body, includeBearerToken: true);
        }

        public Task<JObject> AcceptHeroRecruitCandidateAsync(string candidateId)
        {
            if (string.IsNullOrWhiteSpace(candidateId))
            {
                throw new ArgumentException("candidateId is required", nameof(candidateId));
            }

            var body = new JObject
            {
                ["candidateId"] = candidateId.Trim()
            };

            return PostJsonAsync(BuildUrl("/api/heroes/recruit/accept"), body, includeBearerToken: true);
        }

        public Task<JObject> DismissHeroRecruitCandidatesAsync()
        {
            return PostJsonAsync(BuildUrl("/api/heroes/recruit/dismiss"), new JObject(), includeBearerToken: true);
        }

        public Task<JObject> ReinforceArmyAsync(string armyId = null)
        {
            var body = new JObject();
            if (!string.IsNullOrWhiteSpace(armyId))
            {
                body["armyId"] = armyId.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/armies/reinforce"), body, includeBearerToken: true);
        }

        public Task<JObject> RenameArmyAsync(string armyId, string name)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                throw new ArgumentException("armyId is required", nameof(armyId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name is required", nameof(name));
            }

            var body = new JObject
            {
                ["armyId"] = armyId.Trim(),
                ["name"] = name.Trim(),
            };

            return PostJsonAsync(BuildUrl("/api/armies/rename"), body, includeBearerToken: true);
        }

        public Task<JObject> SplitArmyAsync(string armyId, int size, string newName = null)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                throw new ArgumentException("armyId is required", nameof(armyId));
            }

            if (size <= 0)
            {
                throw new ArgumentException("size must be greater than zero", nameof(size));
            }

            var body = new JObject
            {
                ["armyId"] = armyId.Trim(),
                ["size"] = size,
            };

            if (!string.IsNullOrWhiteSpace(newName))
            {
                body["newName"] = newName.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/armies/split"), body, includeBearerToken: true);
        }

        public Task<JObject> MergeArmyAsync(string sourceArmyId, string targetArmyId)
        {
            if (string.IsNullOrWhiteSpace(sourceArmyId))
            {
                throw new ArgumentException("sourceArmyId is required", nameof(sourceArmyId));
            }

            if (string.IsNullOrWhiteSpace(targetArmyId))
            {
                throw new ArgumentException("targetArmyId is required", nameof(targetArmyId));
            }

            var body = new JObject
            {
                ["sourceArmyId"] = sourceArmyId.Trim(),
                ["targetArmyId"] = targetArmyId.Trim(),
            };

            return PostJsonAsync(BuildUrl("/api/armies/merge"), body, includeBearerToken: true);
        }

        public Task<JObject> DisbandArmyAsync(string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                throw new ArgumentException("armyId is required", nameof(armyId));
            }

            var body = new JObject
            {
                ["armyId"] = armyId.Trim(),
            };

            return PostJsonAsync(BuildUrl("/api/armies/disband"), body, includeBearerToken: true);
        }

        public Task<JObject> AssignArmyHoldAsync(string armyId, string regionId, string posture = null)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                throw new ArgumentException("armyId is required", nameof(armyId));
            }

            if (string.IsNullOrWhiteSpace(regionId))
            {
                throw new ArgumentException("regionId is required", nameof(regionId));
            }

            var body = new JObject
            {
                ["armyId"] = armyId.Trim(),
                ["regionId"] = regionId.Trim(),
            };

            if (!string.IsNullOrWhiteSpace(posture))
            {
                body["posture"] = posture.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/armies/hold"), body, includeBearerToken: true);
        }

        public Task<JObject> StartWarfrontAssaultAsync(string regionId, string armyId = null, string heroId = null)
        {
            if (string.IsNullOrWhiteSpace(regionId))
            {
                throw new ArgumentException("regionId is required", nameof(regionId));
            }

            var body = new JObject
            {
                ["regionId"] = regionId.Trim(),
            };

            if (!string.IsNullOrWhiteSpace(armyId))
            {
                body["armyId"] = armyId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(heroId))
            {
                body["heroId"] = heroId.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/warfront/assault"), body, includeBearerToken: true);
        }

        public Task<JObject> StartGarrisonStrikeAsync(string regionId, string armyId = null, string heroId = null)
        {
            if (string.IsNullOrWhiteSpace(regionId))
            {
                throw new ArgumentException("regionId is required", nameof(regionId));
            }

            var body = new JObject
            {
                ["regionId"] = regionId.Trim(),
            };

            if (!string.IsNullOrWhiteSpace(armyId))
            {
                body["armyId"] = armyId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(heroId))
            {
                body["heroId"] = heroId.Trim();
            }

            return PostJsonAsync(BuildUrl("/api/garrisons/strike"), body, includeBearerToken: true);
        }

        public Task<JObject> ReleaseArmyHoldAsync(string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                throw new ArgumentException("armyId is required", nameof(armyId));
            }

            var body = new JObject
            {
                ["armyId"] = armyId.Trim(),
            };

            return PostJsonAsync(BuildUrl("/api/armies/release_hold"), body, includeBearerToken: true);
        }

        private string BuildUrl(string path)
        {
            var baseUrl = getHttpBaseUrl()?.Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("HTTP base URL is not configured.");
            }

            return baseUrl.TrimEnd('/') + path;
        }

        private async Task<JObject> GetJsonAsync(string url)
        {
            using var request = UnityWebRequest.Get(url);
            ApplyHeaders(request, includeBearerToken: true);
            await SendAsync(request);
            return ParseResponse(request);
        }

        private async Task<JObject> PostJsonAsync(string url, JObject body, bool includeBearerToken)
        {
            var bytes = Encoding.UTF8.GetBytes(body.ToString());
            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(bytes),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, includeBearerToken);
            await SendAsync(request);
            return ParseResponse(request);
        }

        private void ApplyHeaders(UnityWebRequest request, bool includeBearerToken)
        {
            request.SetRequestHeader("Accept", "application/json");
            if (!includeBearerToken)
            {
                return;
            }

            var token = getBearerToken?.Invoke();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token.Trim()}");
            }
        }

        private static async Task SendAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }

        private static JObject ParseResponse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"HTTP {request.responseCode}: {request.error}\n{request.downloadHandler?.text}");
            }

            var text = request.downloadHandler?.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new JObject();
            }

            return JObject.Parse(text);
        }
    }
}
