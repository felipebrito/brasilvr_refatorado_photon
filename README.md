# Brasil VR - Photon Refatorado

Este repositório contém o sistema completo de Realidade Virtual desenvolvido para exibição de vídeos em 360 graus, sincronizado em tempo real. O sistema é dividido em duas aplicações que se comunicam de forma robusta e descentralizada através da rede usando o **Photon PUN 2**.

## 🏗️ Estrutura do Projeto

O sistema é composto por dois projetos Unity distintos que operam em conjunto:

### 1. Tablet Controller (`Tablet_Controller/`)
- **Plataforma:** Android (Tablet)
- **Função:** Atua como o "Mestre" (Master Client) da rede. Ele cria a sala no servidor do Photon e gerencia as sessões de vídeo.
- **Interface:** Exibe o status ao vivo dos 4 possíveis usuários (óculos) conectados. Permite ao operador selecionar qual usuário vai assistir a qual vídeo.
- **Preview Local:** Utiliza versões "Proxy" (`_PT.mp4`, com qualidade reduzida) dos vídeos. Quando um vídeo é enviado para um usuário, o Tablet exibe uma miniatura local do vídeo rodando em perfeita sincronia com o óculos VR.
- **Controle Total:** Possui botões de **Play**, **Pause**, **Stop** e controle da **Timeline** que refletem instantaneamente nos óculos VR (além de controlar a miniatura no próprio tablet).

### 2. Oculus VR Player (`Oculus_VR_Player/`)
- **Plataforma:** Android (Oculus Quest 2/3)
- **Função:** Atuam como os "Clientes" da rede. Eles se conectam na sala criada pelo Tablet e aguardam comandos.
- **Identificação Automática:** Cada óculos é alocado em um "slot" específico (Player 1 a 4) de forma dinâmica, baseando-se no `appId` gerado no build (`com.brasilvr1` até `com.brasilvr4`).
- **Sincronização RPC:** Recebe comandos instantâneos via chamadas RPC do Photon e espelha as propriedades customizadas da sala, garantindo que o vídeo comece, pause ou avance no tempo exato.

---

## 🚀 Novidades e Melhorias Recentes (v3 - Robustez e Estabilidade)

O sistema passou por uma grande refatoração de código para garantir extrema resiliência e fail-safe durante os eventos:

- **Modo "Single-Oculus Fallback":** Para facilitar testes e operações rápidas com apenas 1 óculos, o sistema agora conta com uma inteligência que identifica se apenas um dispositivo VR está conectado. Caso positivo, ele ignora falhas de seleção na UI do Tablet (toques no painel errado) e força o envio do comando diretamente para o óculos ativo.
- **Sincronização Tardia de Propriedades (Late-Join Sync):** Corrigido o bug onde óculos que demoravam para enviar o `SlotIndex` eram ignorados pelo Tablet. O sistema agora escuta ativamente o `OnPlayerPropertiesUpdate` e acomoda os jogadores nos slots corretos assim que as propriedades de rede são recebidas, evitando deadlocks na UI.
- **Time Sync & Slider:** Adicionado controle em tempo real da linha do tempo do vídeo (Timeline Slider). O operador pode arrastar o slider no Tablet e o vídeo pulará instantaneamente para aquele ponto nos óculos VR correspondentes.
- **Envio Global vs Individual:** A interface suporta o disparo de um vídeo para apenas um usuário específico ou o disparo global para todos os óculos simultaneamente.
- **Correção da Tela Preta:** A miniatura no Tablet agora busca dinamicamente a textura gerada pelo reprodutor de vídeo, impedindo que o preview fique preto. Os vídeos proxy (480x240, 64kbps) rodam nativamente no Tablet para aliviar o consumo de bateria e CPU.

---

## 🛠️ Como Compilar e Testar

### Scripts Automatizados (Mac/Linux)
A raiz do repositório conta com diversos scripts `.sh` para facilitar os builds e evitar que o Unity trave devido a múltiplas instâncias:

- `./build_tablet.sh` - Compila o APK do Tablet.
- `./build_vr3.sh` - Compila especificamente o APK para o Óculos 3 (usando o appId e configurações corretas).
- `./build_4_versions.sh` - Compila de uma vez todos os 4 APKs dos óculos.

*(Nota: Certifique-se de que o Unity Hub não esteja com os projetos abertos enquanto roda os scripts de batchmode, ou o build falhará por instâncias duplicadas).*

### Fluxo de Teste Manual
1. **Tablet:** Execute o aplicativo no Tablet. Ele deve indicar que a sala está criada.
2. **Óculos:** Coloque o Óculos, inicie o app VR. Em poucos segundos ele se conectará na sala e aparecerá como "Online" no Tablet, no respectivo slot.
3. **Play:** Toque no vídeo desejado no painel do Tablet e veja o vídeo rodar instantaneamente em 360º no VR.

---
*Projeto mantido e desenvolvido sob demanda para otimizar exibições simultâneas em feiras e eventos.*
