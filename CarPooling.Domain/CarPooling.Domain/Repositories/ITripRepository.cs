using CarPooling.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Domain.Repositories
{
    public interface ITripRepository
    {
        Task<int> create(Trip trip);
    }
}
