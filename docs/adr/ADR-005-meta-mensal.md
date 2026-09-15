# ADR-005: Cálculo da Meta Mensal por Acúmulo de Pontos no Mês Vigente

**Status:** Aceito

## Contexto

A regra de negócio §1.2.5 do `prd-adr.md` define que a meta mensal deve acompanhar "o total de pontos **acumulados** no ciclo mensal vigente". No entanto, a implementação de referência (§5.1) calculava o progresso da barra pelo **saldo atual** (`pointsBalance`).

Como o saldo diminui a cada resgate (`Spent`), usar o saldo corrente como progresso da meta faz a barra **regredir** após uma troca de recompensa — comportamento indesejado e divergente da regra de negócio.

## Decisão

O progresso da meta mensal é calculado pelo **somatório dos pontos ganhos (`Type = Earned`) no mês vigente**, extraído do histórico imutável (`PointsHistory`):

- Filtro: `type == Earned` e `createdAt` no mesmo mês/ano atual.
- Progresso = `acumuladoEarned / MonthlyGoalPoints` (limitado a 100%).
- O saldo (`pointsBalance`) continua sendo usado para saldo disponível e validação de resgate.

**Implementação:** `frontend/src/app/page.tsx` (função `monthlyEarned`, usando `isCurrentMonth`). O backend não precisa de estado extra, pois o histórico é a fonte de verdade.

## Consequências

- A barra de meta não regride ao resgatar recompensas.
- Fiel à regra de negócio "pontos acumulados no ciclo mensal vigente".
- Custo: uma consulta adicional ao histórico no dashboard da criança (aceitável no MVP; pode virar endpoint dedicado no backend se necessário).