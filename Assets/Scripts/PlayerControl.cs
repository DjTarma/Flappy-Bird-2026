using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    [Header("Movimento e Vento")]
    public float forcaMovimento = 5f;
    public float forcaDoVento = 3f;
    public float limiteEsquerda = -10f;
    public float limiteDireita = -2f;

    [Header("Movimento Vertical (W e S)")]
    public float velocidadeVertical = 5f;
    public float limiteTopoY = 4f;
    public float limiteBaseY = -4f;

    [Header("Aparência dos Tiros")]
    public Sprite imagemTiroSimples;
    public Sprite imagemTiroPoderoso;
    public Vector3 escalaTiroSimples = new Vector3(0.3f, 0.3f, 1f);
    public Vector3 escalaTiroPoderoso = new Vector3(1f, 1f, 1f);

    [Header("Combate - Tiro Simples (Botão Esquerdo)")]
    public float velocidadeProjetilNormal = 15f;
    public int danoProjetilNormal = 10;
    public float cooldownTiroSimples = 0.5f;
    private float proximoTiroNormal = 0f;

    [Header("Combate - Tiro Poderoso (Botão Direito)")]
    public float velocidadeProjetilGigante = 25f;
    public int danoProjetilGigante = 50;
    public float cooldownGigante = 10f;
    private float tempoUltimoGigante = -10f;

    private SpriteRenderer spriteRenderer;
    private Color corAmareloPisca = new Color(1f, 0.9f, 0.2f, 1f);

    [Header("Vidas e UI")]
    public int vidasMaximas = 3;
    private int vidasAtuais;
    public TextMeshProUGUI textoVidas;
    public TextMeshProUGUI textoGameOver;

    [Header("Score e UI")]
    public int score = 0;
    public int inimigosMortos = 0;
    public int metaParaBoss = 10;
    public TextMeshProUGUI textoScore;

    [Header("Sistema de Boss")]
    public float posicaoXNascimentoBoss = 6.75f;
    private bool bossJaGerado = false;

    private Rigidbody2D corpoJogador;
    private Coroutine rotinaPiscar;
    private bool tomandoDano = false;
    private bool jogoAcabou = false;

    void Start()
    {
        corpoJogador = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        corpoJogador.gravityScale = 0f;
        corpoJogador.freezeRotation = true;

        vidasAtuais = vidasMaximas;
        AtualizarUI();

        if (textoGameOver != null) textoGameOver.gameObject.SetActive(false);

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        if (jogoAcabou)
        {
            if (Input.GetKeyDown(KeyCode.R)) ReiniciarJogo();
            return;
        }

        float movimentoX = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movimentoX = 1f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movimentoX = -1f;

        float novaPosicaoX = transform.position.x;
        novaPosicaoX += movimentoX * forcaMovimento * Time.deltaTime;
        novaPosicaoX -= forcaDoVento * Time.deltaTime;
        novaPosicaoX = Mathf.Clamp(novaPosicaoX, limiteEsquerda, limiteDireita);

        float movimentoY = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movimentoY = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movimentoY = -1f;

        float novaPosicaoY = transform.position.y;
        novaPosicaoY += movimentoY * velocidadeVertical * Time.deltaTime;
        novaPosicaoY = Mathf.Clamp(novaPosicaoY, limiteBaseY, limiteTopoY);

        transform.position = new Vector3(novaPosicaoX, novaPosicaoY, transform.position.z);

        if (Input.GetButton("Fire1") && Time.time >= proximoTiroNormal)
        {
            AtirarNormal();
            proximoTiroNormal = Time.time + cooldownTiroSimples;
        }

        if (Input.GetButtonDown("Fire2") && Time.time >= tempoUltimoGigante + cooldownGigante)
        {
            AtirarGigante();
        }

        if (!tomandoDano)
        {
            if (rotinaPiscar != null) { }
            else if (Input.GetButton("Fire1")) spriteRenderer.color = corAmareloPisca;
            else if (Time.time < tempoUltimoGigante + cooldownGigante) spriteRenderer.color = new Color(1f, 1f, 0.6f, 1f);
            else spriteRenderer.color = Color.white;
        }
    }

    public void GameOver(bool vitoria = false)
    {
        jogoAcabou = true;
        if (textoGameOver != null)
        {
            textoGameOver.text = vitoria ? "VITÓRIA!\n\nAperte R para Reiniciar" : "GAME OVER\n\nAperte R para Reiniciar";
            textoGameOver.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    void ReiniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void AtirarNormal() { CriarTiro(imagemTiroSimples, danoProjetilNormal, velocidadeProjetilNormal, escalaTiroSimples); }

    void AtirarGigante()
    {
        tempoUltimoGigante = Time.time;
        CriarTiro(imagemTiroPoderoso, danoProjetilGigante, velocidadeProjetilGigante, escalaTiroPoderoso);
        if (!tomandoDano)
        {
            if (rotinaPiscar != null) StopCoroutine(rotinaPiscar);
            rotinaPiscar = StartCoroutine(RotinaPiscar(3f));
        }
    }

    void CriarTiro(Sprite imagem, int dano, float velocidade, Vector3 escala)
    {
        if (imagem == null) return;

        GameObject novoTiro = new GameObject("TiroGerado");
        SpriteRenderer sr = novoTiro.AddComponent<SpriteRenderer>();
        sr.sprite = imagem;
        sr.sortingOrder = 5; // Garante que o tiro do player fica visível
        novoTiro.transform.localScale = escala;

        BoxCollider2D colisor = novoTiro.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;

        Projetil script = novoTiro.AddComponent<Projetil>();
        script.dano = dano;
        script.velocidade = velocidade;
        // LINHA REMOVIDA AQUI (script.direcao = 1f)

        novoTiro.transform.position = transform.position;
    }

    IEnumerator RotinaPiscar(float duracao)
    {
        float tempoFim = Time.time + duracao;
        while (Time.time < tempoFim)
        {
            if (!tomandoDano) spriteRenderer.color = corAmareloPisca;
            yield return new WaitForSeconds(0.1f);
            if (!tomandoDano) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        rotinaPiscar = null;
    }

    void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.gameObject.CompareTag("Abismo")) ReceberDano(3);
    }

    public void ReceberDano(int dano)
    {
        if (jogoAcabou) return;

        vidasAtuais -= dano;
        AtualizarUI();
        StartCoroutine(EfeitoDano());

        if (vidasAtuais <= 0) GameOver(false);
    }

    IEnumerator EfeitoDano()
    {
        tomandoDano = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        tomandoDano = false;
    }

    public void AdicionarMorte()
    {
        inimigosMortos++;
        score += 100;
        AtualizarUI();

        // O Boss já está na cena, aqui ele vai se mover para a tela visível do jogador.
        if (inimigosMortos >= metaParaBoss && !bossJaGerado)
        {
            bossJaGerado = true;
            GameObject bossNaCena = GameObject.FindGameObjectWithTag("Boss");
            if (bossNaCena != null)
            {
                BossControl scriptBoss = bossNaCena.GetComponent<BossControl>();
                if (scriptBoss != null) scriptBoss.AtivarBoss();
            }
        }
    }

    void AtualizarUI()
    {
        if (textoVidas != null) textoVidas.text = "Felpudo: " + vidasAtuais.ToString();
        if (textoScore != null) textoScore.text = "Placar: " + score.ToString();
    }
}