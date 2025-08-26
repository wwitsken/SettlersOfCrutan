namespace SettlersOfCrutan.Infrastructure.Redis;

public static class Scripts
{
    // KEYS[1] = key
    // ARGV[1] = expectedVersion (string)
    // ARGV[2] = newJson
    // Returns: 1 if write happened, 0 otherwise
    public const string CompareExchangeSet =
@"
local key = KEYS[1]
local expected = ARGV[1]
local newJson = ARGV[2]

local cur = redis.call('GET', key)
if cur == false then
  -- new insert must expect version 0
  if expected ~= '0' then return 0 end
  redis.call('SET', key, newJson)
  return 1
end

-- find ""version"":<num> in existing JSON (simple parse; keep JSON lean)
local curVersion = string.match(cur, '""version""%s*:%s*(%d+)')
if curVersion == nil then return 0 end
if tostring(curVersion) ~= expected then return 0 end

redis.call('SET', key, newJson)
return 1
";
}
