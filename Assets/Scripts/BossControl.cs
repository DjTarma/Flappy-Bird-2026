using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossControl : MonoBehaviour
{
    [Header("Status do Boss")]
    public int vidaMaxima = 100;
    private int vidaAtual;
    public Slider barraDeVidaBoss;

    [Header("Movimento")]
    public float velocidadeMovimento = 3f;
    public float posicaoInicialX = 12f; // Decidi deixar fora da tela, pois o boss vai andar até o limiteX, que está abaixo.
    public float limiteX = 6.75f;
    public float limiteY = 3.5f;

    [Header("Tiro Rápido (Pequeno)")]
    public Sprite imagemTiroRapido;
    public int danoProjetilRapido = 1;
    public float velocidadeProjetilRapido = 15f;
    public float escalaProjetilRapido = 0.5f;
    public float cooldownTiroRapido = 0.5f; // Tempo entre cada tiro rápido (meio segundo, viu? Vai ficar um Rio de Janeiro)
    private float proximoTiroRapido;

    [Header("Tiro Poderoso (Gigante)")]
    public Sprite imagemTiroPoderoso;
    public int danoProjetilPoderoso = 1;
    public float velocidadeProjetilPoderoso = 10f;
    public float escalaProjetilPoderoso = 1.5f;
    public float cooldownTiroPoderoso = 5f; // Tempo entre cada tiro gigante (5 segundos nessa poha)
    private float proximoTiroPoderoso;

    private SpriteRenderer spriteRenderer;
    private Vector2 posicaoAlvo;
    private PlayerControl scriptJogador;

    private bool bossAtivo = false;
    private bool chegouNaPosicao = false;

    void Start()
    {
        vidaAtual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.position = new Vector3(posicaoInicialX, 0f, 0f);

        if (barraDeVidaBoss != null)
        {
            barraDeVidaBoss.maxValue = vidaMaxima;
            barraDeVidaBoss.value = vidaAtual;
            barraDeVidaBoss.gameObject.SetActive(false);
        }

        GameObject jogadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jogadorObj != null) scriptJogador = jogadorObj.GetComponent<PlayerControl>();
    }

    public void AtivarBoss() // Após a flag de 5 criaturas serem mortas, o boss aparece.
    {
        bossAtivo = true;
        GeradorInimigos.pararDeGerar = true; // O prefab dos inimigos não vai 

        if (barraDeVidaBoss != null) barraDeVidaBoss.gameObject.SetActive(true);

        posicaoAlvo = new Vector2(limiteX, 0f);
    }

    void Update()
    {
        if (!bossAtivo) return;

        // --- Movimento do boss e também mira dos projéteis---
        if (!chegouNaPosicao)
        {
            transform.position = Vector2.MoveTowards(transform.position, posicaoAlvo, velocidadeMovimento * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - limiteX) < 0.1f)
            {
                chegouNaPosicao = true;
                // Inicia os temporizadores quando ele chega na posição de luta
                proximoTiroRapido = Time.time + 1f;
                proximoTiroPoderoso = Time.time + 2.5f;
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, posicaoAlvo) < 0.5f)
            {
                float novoY = Random.Range(-limiteY, limiteY);
                posicaoAlvo = new Vector2(limiteX, novoY);
            }
            transform.position = Vector2.MoveTowards(transform.position, posicaoAlvo, velocidadeMovimento * Time.deltaTime);

            // Sistema de projéteis independentes pra o Boss disparar contra o player

            // 1. Tiro rápido (A cada 0.5s)
            if (Time.time >= proximoTiroRapido)
            {
                DispararTiro(imagemTiroRapido, danoProjetilRapido, velocidadeProjetilRapido, escalaProjetilRapido);
                proximoTiroRapido = Time.time + cooldownTiroRapido;
            }

            // 2. Tiro poderoso (A cada 5s)
            if (Time.time >= proximoTiroPoderoso)
            {
                DispararTiro(imagemTiroPoderoso, danoProjetilPoderoso, velocidadeProjetilPoderoso, escalaProjetilPoderoso);
                proximoTiroPoderoso = Time.time + cooldownTiroPoderoso;
            }
        }
    }

    // Função de disparo dos projéteis do boss, um pequeno e outro grande. Confira se o inspector do Unity vai mostrar a escala da imagem editável, pls!!!
    // Não vacile em deixar de conferir se estará funcional.
    void DispararTiro(Sprite imagem, int dano, float velocidade, float escala)
    {
        if (imagem == null || scriptJogador == null) return;

        GameObject tiro = new GameObject("TiroBoss");
        SpriteRenderer sr = tiro.AddComponent<SpriteRenderer>();
        sr.sprite = imagem;
        sr.sortingOrder = 10;
        tiro.transform.localScale = new Vector3(escala, escala, 1);

        BoxCollider2D col = tiro.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        BossProjectile scriptTiro = tiro.AddComponent<BossProjectile>();
        scriptTiro.dano = dano;
        scriptTiro.velocidade = velocidade;

        Vector3 posicaoNascimento = transform.position + new Vector3(-1f, 0f, 0f);

        // O tiro sempre mira no jogador (Conforme a velocidade dele, editável pelo Inspector do Unity)
        Vector2 direcao = (scriptJogador.transform.position - posicaoNascimento).normalized;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        tiro.transform.rotation = Quaternion.Euler(0, 0, angulo);

        // Solução para o código fazer o boss disparar os projéteis para a direção do jogador sem olhar para a direita.
        sr.flipX = true;
        sr.flipY = true;

        tiro.transform.position = posicaoNascimento; // Centro esquerda do Boss.
    }

    public void ReceberDano(int dano) // Aqui é onde ele recebe dano, e reage com o Slider de HP dele. Embaixo está o 
    {
        if (!bossAtivo) return;

        vidaAtual -= dano;
        if (barraDeVidaBoss != null) barraDeVidaBoss.value = vidaAtual;

        StartCoroutine(PiscarDano());

        if (vidaAtual <= 0) Morrer();
    }

    IEnumerator PiscarDano() // O Boss ao receber dano, ele vai brilhar vermelho. Confira o índice de cor definido no spriteRenderer...
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Morrer() // Boss morre e a condição de vitória é definida.
    {
        if (scriptJogador != null) scriptJogador.GameOver(true);
        if (barraDeVidaBoss != null) barraDeVidaBoss.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}