# Jogo por Turnos - Redes

## Autoria

- António Rodrigues, 22202884

## Introdução

Este jogo é um jogo de tiros jogado por turnos, numa grelha, em que cada jogador escolhe no seu turno se quer andar e disparar.
O jogo usa a package [Unity Netcode for GameObjects](https://docs.unity3d.com/Manual/com.unity.netcode.gameobjects.html) e a package [Relay](https://docs.unity3d.com/Manual/com.unity.services.relay.html) também do Unity.
Usa como base o código feito em aula com o Professor Diogo Andrade.

## Descrição Técnica

### Jogo

Ao abrir temos duas opções, iniciar um servidor ou entrar num como jogador:

[First screen](./Images/first_view.png)

Quando se clica para iniciar o servidor, aparece este ecrã, com o código para os jogador se conectarem ao lado:

[Server view](./Images/server_view_before_players.png)

Um jogador ao pôr esse código no campo por baixo do botão e ao clicar entra no jogo:

[Jogador conectado](./Images/player_connecting.png)

Vista lado a lado do servidor (esquerda) e do jogador (direita).
O jogador tem no seu UI o icon do personagem que está a controlar, e por baixo o movimento que decidiu tomar nessa ronda.
O servidor tem um botão para começar o jogo mais cedo e ignorar o temporizador de 30 segundos.
De resto vêm o mesmo.

[Side by side](./Images/side_by_side_player_server.png)

Os jogadores escolhem o seu movimento com:
- W: Andar para cima;
- S: Andar para baixo;
- D: Andar para a direita;
- A: Andar para a esquerda;
- Espaço: Disparar na direção do último movimento escolhido.

Esses movimentos são dispostos por baixo do icon do jogador, com um icon da decisão, quando se despara a bala fica da cor da direção em que vai:

[During game](./Images/move_up.png)
[During game](./Images/move_right.png)
[During game](./Images/shoot_right.png)

Durante o jogo cada jogador tem 2 segundos para escolher a sua próxima ação e depois essa ação é executada.

### 