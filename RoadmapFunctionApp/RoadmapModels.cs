using System;
using System.Collections.Generic;

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

    public class UserInfo
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
    }
}
