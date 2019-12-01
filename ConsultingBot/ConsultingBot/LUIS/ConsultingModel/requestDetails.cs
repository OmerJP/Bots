﻿using ConsultingData.Models;
using System.Collections.Generic;

namespace ConsultingBot
{
    public enum Intent
    {
        Unknown,
        AddToProject,
        BillToProject,
    }

    public class Person
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class RequestDetails
    {
        public Intent intent { get; set; } = Intent.Unknown;

        // LUIS entity values
        public string projectName { get; set; } = null;
        public string personName { get; set; } = null;
        public double workHours { get; set; } = 0.0;
        public string workDate { get; set; } = null;

        // Intermediate results
        public List<ConsultingProject> possibleProjects { get; set; } = new List<ConsultingProject>();
        public List<Person> possiblePersons { get; set; } = new List<Person>();

        // Resolved value validated with data
        public ConsultingProject project { get; set; } = null;
        public Person person { get; set; } = null;
    }
}
