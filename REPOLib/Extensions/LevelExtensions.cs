using System.Collections.Generic;
using UnityEngine;

namespace REPOLib.Extensions;


internal static class LevelExtensions
{
    public static IEnumerable<GameObject> GetAllModules(this Level lvl) => 
    [
        ..lvl.ModulesExtraction1,
        ..lvl.ModulesExtraction2,
        ..lvl.ModulesExtraction3,
        ..lvl.ModulesNormal1,
        ..lvl.ModulesNormal2,
        ..lvl.ModulesNormal3,
        ..lvl.ModulesPassage1,
        ..lvl.ModulesPassage2,
        ..lvl.ModulesPassage3,
        ..lvl.ModulesDeadEnd1,
        ..lvl.ModulesDeadEnd2,
        ..lvl.ModulesDeadEnd3 
    ];
}