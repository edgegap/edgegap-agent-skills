// Runs on the PlayFab rule for playfab.matchmaking.match_found.
const { app } = require('@azure/functions');

const TITLE = process.env.PLAYFAB_TITLE_ID;
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function playfab(path, body, headers) {
  const res = await fetch(`https://${TITLE}.playfabapi.com${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...headers },
    body: JSON.stringify(body),
  });
  const json = await res.json();
  if (json.code !== 200) throw new Error(`${path}: ${json.errorMessage}`);
  return json.data;
}

async function edgegap(method, path, body) {
  const res = await fetch(`https://api.edgegap.com${path}`, {
    method,
    headers: { 'Content-Type': 'application/json', Authorization: `token ${process.env.EDGEGAP_API_TOKEN}` },
    body: body ? JSON.stringify(body) : undefined,
  });
  const json = await res.json();
  if (!res.ok) throw new Error(`Edgegap ${path} ${res.status}: ${JSON.stringify(json)}`);
  return json;
}

app.http('StartEdgegapDeployment', {
  methods: ['POST'],
  authLevel: 'function',
  handler: async (request, context) => {
    const body = await request.json();
    const payload = body.PlayStreamEventEnvelope?.EventData ?? {};
    const { MatchId, QueueName } = payload;
    if (!MatchId || !QueueName) {
      context.log('StartEdgegapDeployment: MatchId and QueueName are required');
      return { status: 400 };
    }

    const entityToken = body.TitleAuthenticationContext.EntityToken;
    const match = await playfab('/Match/GetMatch', { QueueName, MatchId }, { 'X-EntityToken': entityToken });
    const profiles = await playfab('/Profile/GetProfiles', { Entities: match.Members.map((m) => m.Entity) }, { 'X-EntityToken': entityToken });
    const playerIds = profiles.Profiles.map((p) => p.Lineage.MasterPlayerAccountId);

    const deployment = await edgegap('POST', '/v2/deployments', {
      application: process.env.EDGEGAP_APP_NAME,
      version: process.env.EDGEGAP_APP_VERSION,
      users: [{ user_type: 'geo_coordinates', user_data: { latitude: 40.7, longitude: -74.0 } }],
      tags: [MatchId.slice(0, 20)],
    });

    // Wait until the server is up, then give every player its address.
    for (let i = 0; i < 30; i++) {
      await sleep(2000);
      const status = await edgegap('GET', `/v1/status/${deployment.request_id}`);
      if (status.running) {
        const port = status.ports['Game Port'].external;
        for (const id of playerIds) {
          await playfab('/Server/UpdateUserData', { PlayFabId: id, Data: { [MatchId]: `${status.public_ip}:${port}` } },
            { 'X-SecretKey': process.env.PLAYFAB_SECRET_KEY });
        }
        return { status: 200 };
      }
    }
    return { status: 504 };
  },
});
