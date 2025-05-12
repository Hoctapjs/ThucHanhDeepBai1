using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TH_Deeplearning.Models
{
    public class JobTitle
    {
        public int Id { get; private set; } 

        public string Title { get; set; }

        private static int nextId = 1;

        public JobTitle()
        {
            Id = nextId++;
            Title = string.Empty;
        }

        public JobTitle(string title)
        {
            Id = nextId++; 
            Title = title;
        }

        public JobTitle(int id, string title)
        {
            Id = id;
            Title = title;
            if (id >= nextId)
            {
                nextId = id + 1;
            }
        }

    }
}