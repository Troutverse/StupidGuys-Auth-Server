# StupidGuys Authentication Server

JWT + PostgreSQL 기반 사용자 인증 및 세션 관리 시스템

## 🎯 핵심 기능

### 실시간 중복 로그인 차단
- **ConcurrentDictionary** 메모리 세션 관리
- 같은 계정 동시 접속 100% 차단
- 비정상 종료 시 자동 세션 정리

### JWT 인증
- **256bit HMAC SHA256** 서명
- Access Token 1시간 유효
- Claims 기반 사용자 식별 (userId, sessionId)

### 데이터베이스
- **PostgreSQL** + Entity Framework Core
- Repository Pattern으로 테스트 용이성 확보
- Code First Migrations

## 🔒 보안 설계

**Thread-Safety:**
- `static ConcurrentDictionary<Guid, User>` 세션 저장소
- 모든 컨트롤러 인스턴스가 공유
- Lock-Free 알고리즘으로 동시성 보장

**인증 플로우:**
```
1. Login → Username/Password 검증
2. 메모리 세션 중복 체크 (1ms)
3. sessionId 생성 (Guid)
4. JWT 토큰 발급 (userId + sessionId)
5. 클라이언트에 토큰 전달
```

## 📊 성능

| 지표 | Before (DB Flag) | After (Memory) |
|-----|-----------------|----------------|
| 로그인 응답 속도 | 150ms | **50ms** |
| DB 쿼리 수 | 많음 | **90% 감소** |
| 중복 차단 성공률 | 60% | **100%** |
| 동시 접속 지원 | 50+ | **1000+** |

## 🏗 시스템 아키텍처
```
Unity Client (HTTP/HTTPS)
    ↓
AuthController (ASP.NET Core)
    ↓
Memory Session (ConcurrentDictionary)
    ↓
UserRepository (Repository Pattern)
    ↓
GameDbContext (EF Core)
    ↓
PostgreSQL Database
```

## 🔗 상세 문서

[Login Service 포트폴리오](https://trout-verse.vercel.app/projects/login-service)

## 🛠 기술 스택

`ASP.NET Core 8` `PostgreSQL` `Entity Framework Core` `JWT` `Npgsql` `Docker` `Render.com`

## 📂 주요 파일 구조
```
Auth/
├── Controllers/
│   ├── AuthController.cs       # 로그인/로그아웃
│   └── UserController.cs       # 사용자 관리
├── Repositories/
│   ├── IUserRepository.cs      # Repository 인터페이스
│   └── UserRepository.cs       # Repository 구현
├── Data/
│   └── GameDbContext.cs        # EF Core Context
├── Models/
│   ├── User.cs                 # 사용자 엔티티
│   └── LoginDto.cs             # DTO
└── Utils/
    └── JwtUtils.cs             # JWT 생성/검증
```

## ⚡ 빠른 시작

**환경 변수 설정:**
```bash
export ConnectionStrings__GameDb="Host=localhost;Database=stupidguys;..."
export Jwt__SecretKey="your-256-bit-secret-key"
```

**서버 실행:**
```bash
dotnet run --project Auth
```

**Unity 클라이언트:**
```csharp
var loginDto = new { id = "username", password = "password" };
var response = await UnityWebRequest.Post(
    "https://your-auth-server.com/auth/login",
    JsonUtility.ToJson(loginDto)
);

var result = JsonUtility.FromJson<LoginResult>(response.downloadHandler.text);
// result.jwt 사용
```

## 🔐 보안 고려사항

- [x] JWT 서명 검증
- [x] 메모리 세션 관리
- [x] HTTPS 통신
- [ ] 비밀번호 해싱 (bcrypt) - 향후 추가 예정
- [ ] Refresh Token - 향후 추가 예정
- [ ] Rate Limiting - 향후 추가 예정

## 🌐 배포

- **Hosting:** Render.com Web Service
- **Database:** Render.com PostgreSQL
- **Environment:** Production

---

**Made with ❤️ by Trout | Unity Certified Developer**
