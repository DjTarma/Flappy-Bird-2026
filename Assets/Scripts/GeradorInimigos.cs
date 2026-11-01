using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeradorInimigos : MonoBehaviour
{
    [Header("Configurações de Geração")]
    public GameObject inimigoPrefab;
    public float tempoEntreGeracoes = 3f; // Intervalo entre as gerações dos inimigos.

    [Tooltip("Número máximo de inimigos que podem existir na tela ao mesmo tempo")]
    public int maximoInimigosNaTela = 5; // Decidi deixar assim pra não cozinhar o processador de ninguém.

    [Header("Limites da Tela")]
    public float limiteDireitaX = 12f; // Posição inicial para gerar os inimigos, caminhando para a esquerda, é óbvio.
    public float limiteTopoY = 4.5f; // Limite da posição máxima de spawn vertical.
    public float limiteBaseY = -4.5f; // Limite da posição mínima de spawn vertical.

    public static bool pararDeGerar = false;

    void Start()
    {
        pararDeGerar = false;
        InvokeRepeating("GerarInimigo", 1f, tempoEntreGeracoes); // repetições entre uma criatura gerada OU não, rsrs
    }

    void GerarInimigo()
    {
        // Se a variável 'pararDeGerar' estiver ativa (por causa do boss), não gera nada.
        if (pararDeGerar || inimigoPrefab == null) return;

        // CONTA quantos inimigos existem na cena agora
        int inimigosAtuais = GameObject.FindGameObjectsWithTag("Inimigo").Length;

        // Se a tela já estiver cheia, ele ignora e espera o próximo ciclo.
        if (inimigosAtuais >= maximoInimigosNaTela)
        {
            return;
        }

        // Se tiver espaço na tela, gera um novo inimigo.
        float sorteioY = Random.Range(limiteBaseY, limiteTopoY);
        Vector2 posicaoDeNascimento = new Vector2(limiteDireitaX, sorteioY);
        Instantiate(inimigoPrefab, posicaoDeNascimento, Quaternion.identity);
    }
}