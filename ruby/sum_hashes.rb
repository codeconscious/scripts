def sum_hashes(target_h, source_h)
  source_h.keys.each do |k|
    case source_h[k]
    when Hash then
      target_h[k] = sum_hashes(target_h[k], source_h[k])
    else
      target_h[k] += source_h[k]
    end
  end

  target_h
end

h = { a: 4, b: { b1: 2, b2: 3, b3: { b3a: 4, b3b: 5 } } }
h2 = { a: 51, b: { b1: 64, b2: 74, b3: { b3a: 84, b3b: 94 } } }

summed_h = sum_hashes(h, h2)
p summed_h
