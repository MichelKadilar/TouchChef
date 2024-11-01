using System.Collections.Generic;
using UnityEngine;

public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<IngredientState> requiredIngredients;
    public float timeLimit;
    public int scoreValue;
}