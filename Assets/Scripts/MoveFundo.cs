using UnityEngine;
using System.Collections;

public class MoveFundo : MonoBehaviour {

  float larguraTela;

  SpriteRenderer grafico;

  void Start(){
  
    grafico = GetComponent<SpriteRenderer>();

  }

  void Update(){
      
    float offset = Time.time * 0.5f;

    grafico = GetComponent<SpriteRenderer>();

    grafico.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
 
  }


}