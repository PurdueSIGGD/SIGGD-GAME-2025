function main()
    -- init()
    local my_coroutine = coroutine.create(process)
    while true do
        if coroutine.status(my_coroutine) == 'dead' then break end
        coroutine.resume(my_coroutine)
        print('processing...')
    end
    -- deinit()
end


function process()
    for i=1,100 do
        print('A')
        coroutine.yield()
    end
    for i=1,100 do
        print('B')
        coroutine.yield()
    end
end

main()
