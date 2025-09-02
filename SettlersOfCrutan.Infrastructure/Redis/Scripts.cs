namespace SettlersOfCrutan.Infrastructure.Redis;

public static class Scripts
{
    // KEYS[1] = aggregate key (string key holding JSON snapshot)
    // KEYS[2] = outbox STREAM key (optional; pass "" to disable)
    //
    // ARGV[1] = expectedVersion (string)
    // ARGV[2] = newJson (aggregate snapshot)
    // ARGV[3] = eventsCount (number as string)
    // ARGV[4..] = each event JSON payload to append to outbox (one per event)
    //
    // Returns: 1 if write happened, 0 otherwise
    public const string CompareExchangeSet =
@"
local key = KEYS[1]
local outboxStream = KEYS[2]

local expected = ARGV[1]
local newJson = ARGV[2]
local eventsCount = tonumber(ARGV[3]) or 0

-- helper: safe truthy check for provided keys
local function has_key(k)
  return (k ~= nil and k ~= '' and k ~= false)
end

-- get a timestamp once for this batch (seconds, microseconds)
local now = redis.call('TIME')
local ts = tostring(now[1]) .. '.' .. string.format('%06d', tonumber(now[2]))

local cur = redis.call('GET', key)
if cur == false then
  -- new insert must expect version 0
  if expected ~= '0' then return 0 end

  redis.call('SET', key, newJson)

  -- append outbox messages (STREAM only, if provided)
  if has_key(outboxStream) and eventsCount > 0 then
    for i = 1, eventsCount do
      local payload = ARGV[3 + i]
      redis.call('XADD', outboxStream, '*',
        'aggKey', key,
        'payload', payload,
        'occurredAtUnix', ts
      )
    end
  end

  return 1
end

-- find ""version"":<num> in existing JSON (simple parse; keep JSON lean)
local curVersion = string.match(cur, '""version""%s*:%s*(%d+)')
if curVersion == nil then return 0 end
if tostring(curVersion) ~= expected then return 0 end

redis.call('SET', key, newJson)

-- append outbox messages (STREAM only, if provided)
if has_key(outboxStream) and eventsCount > 0 then
  for i = 1, eventsCount do
    local payload = ARGV[3 + i]
    redis.call('XADD', outboxStream, '*',
      'aggKey', key,
      'payload', payload,
      'occurredAtUnix', ts
    )
  end
end

return 1
";
}
