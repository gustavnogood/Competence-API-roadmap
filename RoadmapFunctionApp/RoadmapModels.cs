using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RoadmapFunctionApp
{
    public class RoadmapRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Role> Roles { get; set; }
    }

    public class Role
    {
        public string RoleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Skill> Skills { get; set; }
    }

    public class Skill
    {
        public string SkillId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class RoadmapResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Role> Roles { get; set; }
    }

    public class UserRequest
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class UserInfo
    {
        
        public string id { get; set; }

        
        public string displayName { get; set; }
    }
}
