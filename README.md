# Brasil VR - Photon Refatorado

Este repositório contém o sistema completo de VR desenvolvido para exibição de vídeos em 360 graus, dividido em duas aplicações que se comunicam através da rede usando o **Photon PUN 2**.

## Estrutura do Projeto

O sistema é composto por dois projetos Unity distintos que operam em conjunto:

### 1. Tablet Controller (`Tablet_Controller/`)
- **Plataforma:** Android (Tablet)
- **Função:** É o "Mestre" (Master Client) da rede. Ele cria a sala no servidor do Photon e gerencia toda a sessão.
- **Interface:** Mostra o status dos 4 usuários (óculos) conectados. Permite ao operador selecionar qual usuário vai assistir a qual vídeo.
- **Preview Local:** Conta com versões "Proxy" (`_PT.mp4`, com qualidade reduzida) dos vídeos. Quando um vídeo é enviado para um usuário, o Tablet exibe uma miniatura local do vídeo rodando em sincronia para que o operador saiba o que o usuário está vendo.
- **Comandos:** Possui botões de **Play**, **Pause** e **Stop** que refletem instantaneamente nos óculos VR (além de pausar/parar a miniatura no próprio tablet).

### 2. Oculus VR Player (`Oculus_VR_Player/`)
- **Plataforma:** Android (Oculus Quest 2/3)
- **Função:** São os "Clientes" da rede. Eles conectam na sala criada pelo Tablet e aguardam comandos.
- **Identificação:** Cada óculos é alocado em um "slot" específico, sendo identificado pelo Mestre como Player 1, Player 2, etc.
- **Sincronização:** Recebe os comandos via chamadas RPC do Photon (`ReceivePlayCommand`, `ReceiveStopCommand`, etc.) e executa as ações em sincronia com o Tablet.

## Funcionalidades de Vídeo (Novas)
- **Play e Pause:** Agora totalmente independentes. O Tablet consegue retomar ou pausar o vídeo do usuário sem reiniciar.
- **Stop (Novo):** Adicionado um comando específico de **Stop**. Ao clicar em Stop no Tablet, o óculos interrompe a reprodução e esconde a esfera de exibição (ficando numa sala de espera). No Tablet, a miniatura também é desligada.
- **Tratamento de Texturas:** A miniatura no Tablet agora busca dinamicamente a `RawImage` dentro dos filhos do GameObject, permitindo que a textura renderizada pelo `UnityMediaPlayer` apareça corretamente na UI sem ficar preta.

## Como Compilar e Testar

1. **Testando no Editor:** Você pode rodar a cena `Tablet.unity` dentro do Editor no projeto `Tablet_Controller`. O Editor criará a sala normalmente e aguardará conexões.
2. **Conectando o Óculos:** Compile o `Oculus_VR_Player` usando o Build Settings (Android). Instale no Óculos via `adb` (`adb install -r <caminho_do_apk>`).
3. **Rede:** Ambas as aplicações devem ter acesso à internet para alcançar os servidores Cloud do Photon. Eles entrarão na mesma sala fixa configurada nos scripts.

## Atualizações Recentes (v2)
- Remoção da tela preta nos previews do Tablet (correção de `GetComponentInChildren<RawImage>`).
- Conversão de vídeos pesados (4K) para arquivos de proxy (480x240, 64kbps áudio) rodando nativamente no Tablet para aliviar o processador.
- Inclusão da lógica de `SendStopCommand()` e `ReceiveStopCommand()` separada da função de Pause.
