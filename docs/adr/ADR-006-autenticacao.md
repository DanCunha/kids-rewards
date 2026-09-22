# ADR-006 — Autenticação com JWT em Cookie HttpOnly

- **Status:** Aceito
- **Data:** 2026-09-20

## Contexto

O MVP original (ADR-004) previa "sem autenticação" (família única, uso local). Com a evolução para múltiplas famílias, é necessário autenticar o usuário (responsável) e controlar o acesso às duas interfaces:

- **Painel Admin** (`/admin`): apenas responsáveis.
- **Dashboard da Criança** (`/`): responsável (com seletor de filhos) ou criança (apenas a própria conta).

## Decisão

1. **JWT em cookie HttpOnly** (`AuthToken`) em vez de `Authorization: Bearer` exposto ao JavaScript. O backend valida o JWT a partir do cookie via `JwtBearerEvents.OnMessageReceived`.
2. **Senha com hash BCrypt** (`BCrypt.Net-Next`); credencial = email + senha.
3. **Dois tipos de usuário** (`UserType`): `Responsible` e `Child`.
   - `Responsible` cadastra crianças vinculadas (`Child.ResponsibleUserId`) e gerencia recompensas próprias (`Reward.OwnerUserId`).
   - `Child` tem vínculo 1:1 com uma `Child` (`User.ChildId`) e **não acessa** o painel admin (política `RequireResponsible`).
4. **Registro público** (`POST /api/auth/register`) cria apenas responsáveis. Logins de criança são criados pelo responsável ao cadastrar a criança (campos opcionais `loginEmail`/`loginPassword`).

## Consequências

- Escopo de dados: crianças, atividades, recompensas e histórico passam a ser filtrados pelo usuário autenticado.
- CORS passou a usar `AllowCredentials()` (cookies cross-origin entre `localhost:3000` e `:5183`).
- Segredo JWT em `appsettings.json` (dev); em produção deve vir de variável de ambiente/segredo.
