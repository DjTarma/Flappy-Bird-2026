using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InimigoControl : MonoBehaviour
{
    [Header("Status")]
    public int vidaMaxima = 30;     // Posso mudar isso no Inspector, por sinal...
    private int vidaAtual;
    public float velocidadeMovimento = 2f;  // Outra coisa que dá pra mudar no inspector.

    private SpriteRenderer spriteRenderer;
    private bool morrendo = false;

    void Start()
    {
        vidaAtual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (morrendo) return; // Se estiver morrendo, para de andar para a esquerda

        transform.position += Vector3.left * velocidadeMovimento * Time.deltaTime;

        // Se sair da tela, some (para não pesar o jogo)
        if (transform.position.x < -15f) Destroy(gameObject);
    }

    public void ReceberDano(int dano)
    {
        if (morrendo) return;

        vidaAtual -= dano;

        // Pisca vermelho ao levar dano
        StartCoroutine(PiscarVermelho());

        if (vidaAtual <= 0) Morrer();
    }

    IEnumerator PiscarVermelho() // Ao receber dano...
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    void Morrer()
    {
        morrendo = true;

        // Avisa o jogador que o inimigo morreu (para contar o Boss)
        GameObject jogador = GameObject.FindGameObjectWithTag("Player");
        if (jogador != null)
        {
            PlayerControl scriptJogador = jogador.GetComponent<PlayerControl>();
            if (scriptJogador != null) scriptJogador.AdicionarMorte();
        }

        // Desliga o colisor para não dar dano no jogador enquanto cai, pra evitar problemas.
        Collider2D colisor = GetComponent<Collider2D>();
        if (colisor != null) colisor.enabled = false;

        // Adiciona física de queda para poder 
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f; // Cai para baixo
        rb.freezeRotation = false; // Deixa ele girar ao cair

        // Fica vermelho escuro ao cair
        if (spriteRenderer != null) spriteRenderer.color = new Color(0.5f, 0f, 0f, 1f);

        // Destrói o objeto após 2 segundos caindo para salvar o processamento.
        Destroy(gameObject, 2f);
    }

    // Colisão com o Jogador
    void OnTriggerEnter2D(Collider2D colisao)
    {
        if (morrendo) return;

        if (colisao.gameObject.CompareTag("Player"))
        {
            colisao.gameObject.GetComponent<PlayerControl>().ReceberDano(1);
            Destroy(gameObject);
        }
    }
}