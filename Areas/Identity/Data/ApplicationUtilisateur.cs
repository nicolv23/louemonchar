using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Projet_Final.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUtilisateur class
public class ApplicationUtilisateur : IdentityUser
{
    [PersonalData]
    [Display(Name = "Prénom")]
    public string FirstName { get; set; }

    [PersonalData]
    [Display(Name = "Nom de famille")]
    public string LastName { get; set; }

}

