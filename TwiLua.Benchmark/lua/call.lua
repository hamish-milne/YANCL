local a = globalFn
local b = 0
for i=1,100000 do
    b = a(b, i)
end
