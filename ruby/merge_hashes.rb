# Recursively sums the values in two hashes. Assumes identical hash structures
# and that all values are either integers or hashes. Returns a new, summed hash.
def merge_hashes(left, right)
  summed = Hash.new(0)

  left.keys.each do |key|
    case left[key]
    when Hash then
      summed[key] = merge_hashes(left[key], right[key])
    else
      summed[key] = left[key] + right[key]
    end
  end

  summed
end

h1 = { a: 4, b: { b1: 2, b2: 3, b3: { b3a: 4, b3b: 5 } } }
h2 = { a: 51, b: { b1: 64, b2: 74, b3: { b3a: 84, b3b: 94 } } }

summed_h = merge_hashes(h1, h2)
p summed_h
