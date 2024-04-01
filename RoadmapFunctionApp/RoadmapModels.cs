using System;
using System.Collections.Generic;

namespace RoadmapFunctionApp
{
    public class RoadmapRequest
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Role> roles { get; set; }
    }

    public class Role
    {
        public string roleId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Skill> skills { get; set; }
    }

    public class Skill
    {
        public string skillId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class RoadmapResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Role> roles { get; set; }
    }
}
