using System.Collections.Generic;
using UnityEngine;

public interface IContainer : IPickable
{
    bool CanAcceptIngredient(BaseIngredient ingredient);
    bool AddIngredient(BaseIngredient ingredient);
    bool RemoveIngredient(BaseIngredient ingredient);
    List<BaseIngredient> GetContents();
    UnityEngine.Transform GetIngredientAttachPoint();  // Spécifié explicitement
}